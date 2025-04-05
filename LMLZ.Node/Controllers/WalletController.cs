using LMLZ.Node.Dto;
using LMLZ.Node.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LMLZ.Node.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WalletController : ControllerBase
{
    private readonly Serilog.ILogger logger = Log.ForContext<WalletController>();
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService) => _walletService = walletService;

    [HttpPost]
    public async Task<ActionResult> CreateWallet([FromQuery] string passphrase)
    {
        await _walletService.CreateNewWalletAsync(passphrase);
        return Created();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WalletDto>>> GetWallets()
        => Ok(await _walletService.GetWalletsAsync());
}
