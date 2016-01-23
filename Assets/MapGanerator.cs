using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class MapGanerator : MonoBehaviour {

	public int Width;

	public int MinHeight;
	public int MaxHeight;

	public int StopProcentage;

	public int MaxDifference;

	[System.Serializable]
	public struct Element{ 
		public int SourceId; 
		public int MaxDeep; 
		public int MinDeep;
		public int Procentage;};

	public Element[] Blocks;
	public Element[] Default;

	public int[,] Map;

	private bool[,] Visitated;

	private struct Coordonates{
		public int x;
		public int y;
	};

	private List<Coordonates> Que = new List<Coordonates>();
	private Coordonates temp;

	private int LastX;

	private int rand;

	void Start()
	{

		Generate ();
		Show ();

	}

	void Show()
	{
		for (int i=0; i<Width; i++) {
			for(int j=0;j<MaxHeight;j++)
			{
				if(Map[j,i]!=0)
					Instantiate( 	 Resources.Load("Prefabs/"+Map[j,i].ToString(), typeof(GameObject)),
					            	new Vector3(i,j),
					            	Quaternion.identity);
			}
		}
	}

	public void Generate()
	{


		LastX = (MinHeight + MaxHeight) / 2;

		Map=new int[MaxHeight+100,Width+10];
		Visitated = new bool[MaxHeight+100,Width+10];

		temp.x = 0;
		temp.y = 0;
		Que.Add (temp);

		Visitated [0, 0] = true;

		while (Que.Count>0) {

			temp=Que[0];

			FindPropperItemFor(temp.x,temp.y);

			if(HaveToPutOnQueue(temp.x + 1 , temp.y))
			{
				temp.x++;
				Visitated[temp.x , temp.y ] = true;
				Que.Add(temp);
			}
			else
			{
				LastX = temp.x;
				temp.x =0;
				temp.y++;
				if(temp.y<Width)
					Que.Add(temp);
			}

			Que.RemoveAt(0);
		}


	}

	bool HaveToPutOnQueue(int x,int y)
	{
		if (x < 0 || y < 0 || x > MaxHeight || y > Width)
			return false;

		if (Map [x - 1, y] == 0)
			return false;

		if (Visitated [x, y])
			return false;

		if (y >= 1 && x>=MinHeight) {
		
			if(Map[x-MaxDifference,y]==0)
				return false;
		
		}


		return true;

	}


	void FindPropperItemFor(int x,int y)
	{
		
		//
		// Select a random propper item for position x,y
		//

		if(MayIStop(x,y))
		{
			rand = Random.Range (0,10000);

			if (rand <= StopProcentage) {
				return;
			}

		}

		rand = Random.Range (0,10000);

		for (int i=0; i<Blocks.Length; i++) {
		
			if(LastX - Blocks[i].MaxDeep <= x && LastX - Blocks[i].MinDeep >= x)
			{
				if(rand <= Blocks[i].Procentage)
				{
					Map[x,y] = Blocks[i].SourceId;
					return;
				}

				rand -= Blocks[i].Procentage;
			}
		
		}

		
		for (int i=0; i<Default.Length; i++) 			
			if(MinHeight - Default[i].MaxDeep <= x && MaxHeight - Default[i].MinDeep >= x)
			{
				Map[x,y] = Default[i].SourceId;
			}

	}

	bool MayIStop(int x,int y)
	{

		if (x < MinHeight)
			return false;

		if (y!=0 && Map [x + MaxDifference, y - 1] != 0)
			return false;

		return true;

	}

}
