using NBitcoin;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockchainHub.BlockExplorer.Models
{
	
	public class TransactionModel
	{
		public string InputAmount
		{
			get;
			set;
		}
		public string OutputAmount
		{
			get;
			set;
		}
		public int? BlockHeight
		{
			get;
			set;
		}
		public uint256 Hash
		{
			get;
			set;
		}

		public int Confirmations
		{
			get;
			set;
		}
		public DateTimeOffset SeenDate
		{
			get;
			set;
		}
		public string FeeRate
		{
			get;
			set;
		}
		public string Fee
		{
			get;
			set;
		}
		public int Version
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
	}
}