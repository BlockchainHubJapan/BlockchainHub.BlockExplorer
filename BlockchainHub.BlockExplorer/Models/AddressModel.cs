using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockchainHub.BlockExplorer.Models
{
	public class AddressModel
	{
		public string Address
		{
			get;
			set;
		}
		public string ConfirmedBalance
		{
			get;
			set;
		}
		public string TotalReceived
		{
			get;
			set;
		}
		public int TransactionCount
		{
			get;
			set;
		}
		public string UnconfirmedBalance
		{
			get;
			set;
		}

		public List<BlockTransactionModel> Transactions
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
}