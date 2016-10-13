using BlockchainHub.BlockExplorer.Models;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlockchainHub.BlockExplorer.Controllers
{
	public class MainController : Controller
	{
		QBitNinjaClient QBit = new QBitNinjaClient(Network.Main);

		[Route("")]
		public async Task<ActionResult> Index(string search = null, int count = 5)
		{
			count = Math.Max(0, count);
			count = Math.Min(50, count);
			if(!string.IsNullOrWhiteSpace(search))
				return View();

			var responses =
				await Task.WhenAll(Enumerable
				.Range(0, count)
				.Select(i => QBit.GetBlock(new BlockFeature(SpecialFeature.Last) { Offset = -i }, true, true))
				.ToArray());

			var model = new MainModel();
			model.NextCount = count + 5;
			foreach(var response in responses.Where(r => r.ExtendedInformation != null && r.AdditionalInformation != null))
			{
				var blockModel = new MainBlockModel();
				blockModel.Hash = response.AdditionalInformation.BlockId;
				blockModel.Height = response.AdditionalInformation.Height;
				blockModel.Size = ToKB(response.ExtendedInformation.Size);
				blockModel.Time = ToRelative(response.AdditionalInformation.BlockTime);
				blockModel.TransactionsCount = response.ExtendedInformation.TransactionCount;
				blockModel.Fees = (response.ExtendedInformation.BlockReward - response.ExtendedInformation.BlockSubsidy).ToDecimal(MoneyUnit.BTC).ToString();
				model.Blocks.Add(blockModel);
			}
			return View(model);
		}

		private string ToRelative(DateTimeOffset time)
		{
			var ago = DateTimeOffset.UtcNow - time;
			if(ago.TotalMinutes < 1.0)
			{
				return (int)(ago.TotalSeconds) + " seconds ago";
			}
			else if(ago.TotalHours < 1.0)
			{
				return (int)(ago.TotalMinutes) + " minutes ago";
			}
			else
			{
				return (int)(ago.TotalHours) + " h, " + (ago.Minutes) + " min ago";
			}
		}

		private string ToKB(int size)
		{
			decimal kb = (decimal)size / 1000.0m;
			kb = decimal.Round(kb, 3);
			return kb + " kB";
		}

		[Route("addresses")]
		public ActionResult Address()
		{
			return View();
		}

		[Route("blocks/{blockId}", Name = "Block")]
		public ActionResult Block(uint256 blockId)
		{
			return View();
		}

		[Route("transactions")]
		public ActionResult Transaction()
		{
			return View();
		}
	}
}