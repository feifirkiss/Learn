using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UCombineSkinnedMgr
{

    /// <summary>
    /// Only for merge materials.
    /// </summary>
	private  int COMBINE_TEXTURE_MAX = 1024;
    private const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";
    private int _TotalWidth =0;
    private int _TotalHeight = 0;

    /// <summary>
    /// Combine SkinnedMeshRenderers together and share one skeleton.
    /// Merge materials will reduce the drawcalls, but it will increase the size of memory. 
    /// </summary>
    /// <param name="skeleton">combine meshes to this skeleton(a gameobject)</param>
    /// <param name="meshes">meshes need to be merged</param>
    /// <param name="combine">merge materials or not</param>
	public void CombineObject(GameObject skeleton, SkinnedMeshRenderer[] meshes, bool combine = false)
    {

        // Fetch all bones of the skeleton
        List<Transform> transforms = new List<Transform>();
        transforms.AddRange(skeleton.GetComponentsInChildren<Transform>(true));

        List<Material> materials = new List<Material>();//the list of materials
        List<CombineInstance> combineInstances = new List<CombineInstance>();//the list of meshes
        List<Transform> bones = new List<Transform>();//the list of bones

        // Below informations only are used for merge materilas(bool combine = true)
        List<Vector2[]> oldUV = null;
        Material newMaterial = null;
        Texture2D newDiffuseTex = null;

        // Collect information from meshes
        for ( int i = 0; i < meshes.Length; i++ )
        {
            SkinnedMeshRenderer smr = meshes[i];
            materials.AddRange(smr.materials); // Collect materials
                                               // Collect meshes
            for ( int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++ )
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = smr.sharedMesh;
                ci.subMeshIndex = sub;
                combineInstances.Add(ci);
            }
            // Collect bones
            for ( int j = 0; j < smr.bones.Length; j++ )
            {
                int tBase = 0;
                for ( tBase = 0; tBase < transforms.Count; tBase++ )
                {
                    if ( smr.bones[j].name.Equals(transforms[tBase].name) )
                    {
                        bones.Add(transforms[tBase]);
                        break;
                    }
                }
            }
        }

        // merge materials
        if ( combine )
        {
            _TotalWidth = 0;
            _TotalHeight = 0;

            newMaterial = new Material(Shader.Find("Mobile/Diffuse"));
            oldUV = new List<Vector2[]>();
            // merge the texture
            List<Texture2D> Textures = new List<Texture2D>();
            for ( int i = 0; i < materials.Count; i++ )
            {
                Textures.Add(materials[i].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D);
                CalculateTotalWidthAndHeight(materials[i]);
            }
            GetUseTextureSize();
            //宽高要为2N次幂 (所有贴图合并到newDiffuseTex这张大贴图上)
            newDiffuseTex = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX, TextureFormat.RGBA32, true);
            Rect[] uvs = newDiffuseTex.PackTextures(Textures.ToArray(), 0,COMBINE_TEXTURE_MAX);
            newMaterial.mainTexture = newDiffuseTex; //更新合并纹理到新材质

            // reset uv 重新计算UV(贴图合并了需要重新计算UV)
            //根据原来单个上的uv算出合并后的uv(uva是单个的，uvb是合并后的)
            Vector2[] uva, uvb;
            for ( int j = 0; j < combineInstances.Count; j++ )
            {
                uva = (Vector2[])( combineInstances[j].mesh.uv );
                uvb = new Vector2[uva.Length];
                for ( int k = 0; k < uva.Length; k++ )
                {
                    uvb[k] = new Vector2(( uva[k].x * uvs[j].width ) + uvs[j].x, ( uva[k].y * uvs[j].height ) + uvs[j].y);
                }
                oldUV.Add(combineInstances[j].mesh.uv);
                combineInstances[j].mesh.uv = uvb;
            }
        }

        // Create a new SkinnedMeshRenderer
        SkinnedMeshRenderer oldSKinned = skeleton.GetComponent<SkinnedMeshRenderer> ();
        if ( oldSKinned != null )
        {

            GameObject.DestroyImmediate(oldSKinned);
        }
        SkinnedMeshRenderer r = skeleton.AddComponent<SkinnedMeshRenderer>();
        r.sharedMesh = new Mesh();
        r.sharedMesh.CombineMeshes(combineInstances.ToArray(), combine, false);// Combine meshes
        r.bones = bones.ToArray();// Use new bones
        if ( combine )
        {
            r.material = newMaterial;//使用新的合并后的材质(设置新材质需重新设置UV)
            for ( int i = 0; i < combineInstances.Count; i++ )
            {
                //这行代码其实并不影响显示，影响显示的是在Mesh合并前的uv
                // 这行的意义在于合并后，再次更换部件时，在新的合并过程中找到正确的单个uv
                //也是oldUV存在的意义
                combineInstances[i].mesh.uv = oldUV[i];
            }
        }
        else
        {
            r.materials = materials.ToArray();
        }
    }

    public void CalculateTotalWidthAndHeight(Material material)
    {
        _TotalWidth += material.mainTexture.width;
        _TotalHeight += material.mainTexture.height;
    }


    public void GetUseTextureSize()
    {
        int tempVal = 0;
        if ( _TotalWidth < COMBINE_TEXTURE_MAX  &&  _TotalHeight < COMBINE_TEXTURE_MAX )
        {
            if ( _TotalWidth < _TotalHeight )
            {
                tempVal = _TotalHeight;

            }
            else
            {
                tempVal = _TotalWidth;
            }
        }
        if ( tempVal >0  )
        {
            COMBINE_TEXTURE_MAX =  Get2Pow(tempVal);
        }
    }

    /// <summary>
    /// 获取最接近输入值的2的N次方的数，最大不会超过1024，例如输入320会得到512
    /// </summary>
    private int Get2Pow(int into)
    {
        int outo = 1;
        for ( int i = 0; i < 10; i++ )
        {
            outo *= 2;
            if ( outo > into )
            {
                break;
            }
        }
        return outo;
    }
}
