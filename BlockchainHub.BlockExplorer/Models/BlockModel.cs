using NBitcoin;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockchainHub.BlockExplorer.Models
{
	public class BlockModel
	{
		public BlockModel()
		{
			Transactions = new List<BlockTransactionModel>();
		}
		public int Height
		{
			get;
			set;
		}
		public DateTimeOffset BlockTime
		{
			get;
			set;
		}
		public int TransactionCount
		{
			get;
			set;
		}
		public double Difficulty
		{
			get;
			set;
		}
		public string Fee
		{
			get;
			set;
		}
		public uint256 Hash
		{
			get;
			set;
		}
		public int Version
		{
			get;
			set;
		}
		public int Confirmations
		{
			get;
			set;
		}
		public uint256 MerkleRoot
		{
			get;
			set;
		}
		public uint256 Previous
		{
			get;
			set;
		}
		public string Size
		{
			get;
			set;
		}
		public string StrippedSize
		{
			get;
			set;
		}
		public int NextCount
		{
			get;
			internal set;
		}

		public List<BlockTransactionModel> Transactions
		{
			get;
			set;
		}
	}

	public class BlockTransactionModel
	{
		public BlockTransactionModel()
		{
			Inputs = new List<BlockTransactionPartModel>();
			Outputs = new List<BlockTransactionPartModel>();
		}

		public bool Income
		{
			get;
			set;
		}
		public uint256 Hash
		{
			get;
			set;
		}
		public string Amount
		{
			get;
			set;
		}
		public string Fee
		{
			get;
			set;
		}
		public bool IsCoinbase
		{
			get;
			set;
		}

		public List<BlockTransactionPartModel> Inputs
		{
			get;
			set;
		}
		public List<BlockTransactionPartModel> Outputs
		{
			get;
			set;
		}
		public int Index
		{
			get;
			internal set;
		}
	}

	public class BlockTransactionPartModel
	{
		public string ScriptPubKey
		{
			get;
			set;
		}
		public string Amount
		{
			get;
			set;
		}
	}
}