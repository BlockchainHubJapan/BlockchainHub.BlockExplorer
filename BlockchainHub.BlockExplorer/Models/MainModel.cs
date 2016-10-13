using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockchainHub.BlockExplorer.Models
{
	public class MainModel
	{
		public MainModel()
		{
			Blocks = new List<MainBlockModel>();
		}
		public List<MainBlockModel> Blocks
		{
			get;
			set;
		}
		public int NextCount
		{
			get;
			set;
		}
	}

	public class MainBlockModel
	{
		public uint256 Hash
		{
			get;
			set;
		}
		public int Height
		{
			get;
			set;
		}
		public string Time
		{
			get;
			set;
		}
		public int TransactionsCount
		{
			get;
			set;
		}
		public string Fees
		{
			get;
			set;
		}
		public string Size
		{
			get;
			set;
		}
	}
}