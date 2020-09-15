using UnityEngine;
using System.Collections.Generic;

//角色管理
public class UCharacterMgr  {

	private UCombineSkinnedMgr skinnedMgr = null;
	public UCombineSkinnedMgr CombineSkinnedMgr { get{ return skinnedMgr; } }

	private int characterIndex = 0;
	private Dictionary<int,UCharacterController> characterDic = new Dictionary<int, UCharacterController>();

	public UCharacterMgr () {

		skinnedMgr = new UCombineSkinnedMgr ();
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skeleton">共用的骨骼prefab名</param>
    /// <param name="weapon"></param>
    /// <param name="head"></param>
    /// <param name="chest"></param>
    /// <param name="hand"></param>
    /// <param name="feet"></param>
    /// <param name="combine"></param>
    /// <returns></returns>
	public UCharacterController Generatecharacter (string skeleton, string weapon, string head, string chest, string hand, string feet, bool combine = false)
	{
		UCharacterController instance = new UCharacterController (characterIndex,skeleton,weapon,head,chest,hand,feet,combine);
		characterDic.Add(characterIndex,instance);
		characterIndex ++;

		return instance;
	}

	public void Removecharacter (CharacterController character)
	{

	}

	public void Update () {

		foreach(UCharacterController character in characterDic.Values)
		{
			character.Update();
		}
	}
}
