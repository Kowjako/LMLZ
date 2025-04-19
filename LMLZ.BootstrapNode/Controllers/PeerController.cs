using LMLZ.BootstrapNode.Model;
using LMLZ.BootstrapNode.Repository;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ILogger = Serilog.ILogger;

namespace LMLZ.BootstrapNode.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PeerController : ControllerBase
{
    private readonly ILogger _logger = Log.ForContext<PeerController>();
    private readonly IPeerRepository _repo;

    public PeerController(IPeerRepository repo)
        => _repo = repo;

    [HttpGet("chunk/{count}")]
    public async Task<ActionResult<IEnumerable<PeerDto>>> GetPeerChunk([FromRoute] int count)
        => Ok(await _repo.GetPeersRandomChunkAsync(count));

    [HttpPost("register")]
    public async Task<ActionResult<Guid>> RegsiterPeer([FromQuery] string port)
    {
        if (HttpContext.Connection.RemoteIpAddress is null)
        {
           _logger.Warning("Remote IP address is null");
            return BadRequest("Remote IP address is null");
        }

        var ip = HttpContext.Connection.RemoteIpAddress!.ToString();
        return Ok(await _repo.AddPeerAsync(ip, port));
    }

    [HttpDelete("unregister")]
    public async Task<ActionResult> UnregisterPeer([FromQuery] Guid id)
    {
        await _repo.RemovePeerAsync(id);
        return NoContent();
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult> Heartbeat([FromQuery] Guid id)
    {
        await _repo.UpdateLastSeenAsync(id);
        return Ok();
    }
}
