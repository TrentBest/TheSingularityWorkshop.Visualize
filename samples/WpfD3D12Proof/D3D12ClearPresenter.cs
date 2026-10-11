using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D12.D3D12;
using static Vortice.DXGI.DXGI;

namespace TheSingularityWorkshop.Visualize.WpfD3D12Proof;

/// <summary>Minimal native Direct3D 12 clear/present loop for validating a WPF-hosted HWND.</summary>
/// <remarks>This intentionally serializes frames for the first correctness proof. It is not a performance benchmark.</remarks>
internal sealed class D3D12ClearPresenter : IDisposable
{
    private const uint BufferCount = 2;
    private readonly IDXGIFactory4 _factory;
    private readonly ID3D12Device _device;
    private readonly ID3D12CommandQueue _queue;
    private readonly ID3D12CommandAllocator _allocator;
    private readonly ID3D12GraphicsCommandList _commandList;
    private readonly ID3D12Fence _fence;
    private readonly AutoResetEvent _fenceEvent = new(false);
    private readonly IDXGISwapChain3 _swapChain;
    private readonly ID3D12DescriptorHeap _rtvHeap;
    private readonly ID3D12Resource[] _backBuffers = new ID3D12Resource[BufferCount];
    private readonly CpuDescriptorHandle[] _rtvHandles = new CpuDescriptorHandle[BufferCount];
    private readonly uint _rtvStride;
    private ulong _fenceValue;
    private bool _disposed;
    private int _width;
    private int _height;

    public D3D12ClearPresenter(nint hwnd, int width, int height)
    {
        if (hwnd == nint.Zero)
            throw new ArgumentException("A valid window handle is required.", nameof(hwnd));

        _width = Math.Max(1, width);
        _height = Math.Max(1, height);
        _factory = CreateDXGIFactory2<IDXGIFactory4>(false);
        D3D12CreateDevice(null, FeatureLevel.Level_11_0, out ID3D12Device? device).CheckError();
        _device = device ?? throw new InvalidOperationException("Direct3D 12 device creation returned no device.");

        _queue = _device.CreateCommandQueue(new CommandQueueDescription
        {
            Type = CommandListType.Direct,
            Flags = CommandQueueFlags.None
        });
        _allocator = _device.CreateCommandAllocator(CommandListType.Direct);
        _commandList = _device.CreateCommandList<ID3D12GraphicsCommandList>(CommandListType.Direct, _allocator);
        _commandList.Close();
        _fence = _device.CreateFence(0);

        var swapChain = _factory.CreateSwapChainForHwnd(_queue, hwnd, new SwapChainDescription1
        {
            Width = (uint)_width,
            Height = (uint)_height,
            Format = Format.R8G8B8A8_UNorm,
            BufferCount = BufferCount,
            BufferUsage = Usage.RenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            SampleDescription = new SampleDescription(1, 0)
        });
        _swapChain = swapChain.QueryInterface<IDXGISwapChain3>();
        swapChain.Dispose();

        _rtvHeap = _device.CreateDescriptorHeap(new DescriptorHeapDescription
        {
            DescriptorCount = BufferCount,
            Type = DescriptorHeapType.RenderTargetView,
            Flags = DescriptorHeapFlags.None
        });
        _rtvStride = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
        var start = _rtvHeap.GetCPUDescriptorHandleForHeapStart();
        for (uint i = 0; i < BufferCount; i++)
        {
            _rtvHandles[i] = start;
            _rtvHandles[i].Ptr = start.Ptr + (nuint)(i * _rtvStride);
            _backBuffers[i] = _swapChain.GetBuffer<ID3D12Resource>(i);
            _device.CreateRenderTargetView(_backBuffers[i], null, _rtvHandles[i]);
        }
    }

    public void Render()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        WaitForGpu();
        var index = _swapChain.CurrentBackBufferIndex;
        var target = _backBuffers[index];
        _allocator.Reset();
        _commandList.Reset(_allocator);

        _commandList.ResourceBarrierTransition(target, ResourceStates.Present, ResourceStates.RenderTarget);
        _commandList.ClearRenderTargetView(_rtvHandles[index], new Color4(0.025f, 0.055f, 0.09f, 1f));
        _commandList.ResourceBarrierTransition(target, ResourceStates.RenderTarget, ResourceStates.Present);
        _commandList.Close();
        _queue.ExecuteCommandList(_commandList);
        _swapChain.Present(1, PresentFlags.None);
        SignalGpu();
    }

    public void Resize(int width, int height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (width == _width && height == _height)
            return;

        WaitForGpu();
        foreach (var buffer in _backBuffers)
            buffer.Dispose();

        _swapChain.ResizeBuffers(BufferCount, (uint)width, (uint)height,
            Format.R8G8B8A8_UNorm, SwapChainFlags.None).CheckError();
        _width = width;
        _height = height;
        var start = _rtvHeap.GetCPUDescriptorHandleForHeapStart();
        for (uint i = 0; i < BufferCount; i++)
        {
            _rtvHandles[i] = new CpuDescriptorHandle(start.Ptr + (nint)(i * _rtvStride));
            _backBuffers[i] = _swapChain.GetBuffer<ID3D12Resource>(i);
            _device.CreateRenderTargetView(_backBuffers[i], null, _rtvHandles[i]);
        }
    }

    private void SignalGpu()
    {
        _fenceValue++;
        _queue.Signal(_fence, _fenceValue).CheckError();
    }

    private void WaitForGpu()
    {
        if (_fence.CompletedValue >= _fenceValue)
            return;
        _fence.SetEventOnCompletion(_fenceValue, _fenceEvent.SafeWaitHandle.DangerousGetHandle()).CheckError();
        _fenceEvent.WaitOne();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        WaitForGpu();
        _disposed = true;
        foreach (var buffer in _backBuffers)
            buffer.Dispose();
        _rtvHeap.Dispose();
        _swapChain.Dispose();
        _fence.Dispose();
        _commandList.Dispose();
        _allocator.Dispose();
        _queue.Dispose();
        _device.Dispose();
        _factory.Dispose();
        _fenceEvent.Dispose();
    }
}
