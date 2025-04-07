using System.Text;
using LMLZ.Node.Dto.Input;
using LMLZ.Node.Dto.Output;
using LMLZ.Node.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMLZ.Node.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService) => _walletService = walletService;

    [HttpPost("create")]
    public async Task<ActionResult> CreateWallet([FromQuery] string passphrase)
    {
        await _walletService.CreateNewWalletAsync(passphrase);
        return Created();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WalletDto>>> GetWallets()
        => Ok(await _walletService.GetWalletsAsync());

    [HttpGet("export/{wallet}")]
    public async Task<ActionResult> ExportWallet([FromRoute] string wallet, [FromQuery] string passPhrase)
    {
        var result = await _walletService.ExportWalletPemAsync(wallet, passPhrase);
        return File(Encoding.UTF8.GetBytes(result), "application/x-pem-file", $"{wallet}.pem");
    }

    [HttpPost("import")]
    public async Task<ActionResult> ImportWallet([FromForm] ImportWalletDto dto)
    {
        await _walletService.ImportWalletUsingPemAsync(dto);
        return Created();
    }
}