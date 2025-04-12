using LMLZ.BootstrapNode.Model;
using LMLZ.BootstrapNode.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LMLZ.BootstrapNode.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PeerController : ControllerBase
{
    private readonly ILogger<PeerController> _logger;
    private readonly IPeerRepository _repo;

    public PeerController(ILogger<PeerController> logger, IPeerRepository repo)
    {
        _logger = logger;
        _repo = repo;
    }

    [HttpGet("chunk/{count}")]
    public async Task<ActionResult<IEnumerable<PeerDto>>> GetPeerChunk([FromRoute] int count)
        => Ok(await _repo.GetPeersRandomChunkAsync(count));

    [HttpPost("register")]
    public async Task<ActionResult<Guid>> RegsiterPeer([FromQuery] string ip, [FromQuery] string port)
        => Ok(await _repo.AddPeerAsync(ip, port));

    [HttpDelete("unregister")]
    public async Task<ActionResult> UnregisterPeer([FromQuery] Guid id)
    {
        await _repo.RemovePeerAsync(id);
        return NoContent();
    }
}
