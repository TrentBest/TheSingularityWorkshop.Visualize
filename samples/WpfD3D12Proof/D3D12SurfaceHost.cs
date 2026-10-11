using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TheSingularityWorkshop.Visualize.WpfD3D12Proof;

/// <summary>Hosts a native child HWND whose swap chain is presented by Direct3D 12.</summary>
public sealed class D3D12SurfaceHost : HwndHost
{
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private nint _childHandle;
    private D3D12ClearPresenter? _presenter;
    private int _lastWidth;
    private int _lastHeight;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _childHandle = CreateWindowEx(0, "static", string.Empty, WsChild | WsVisible,
            0, 0, Math.Max(1, (int)ActualWidth), Math.Max(1, (int)ActualHeight),
            hwndParent.Handle, nint.Zero, GetModuleHandle(null), nint.Zero);
        if (_childHandle == nint.Zero)
            throw new InvalidOperationException("Could not create the native child window for the Direct3D 12 surface.");

        try
        {
            _presenter = new D3D12ClearPresenter(_childHandle,
                Math.Max(1, (int)ActualWidth), Math.Max(1, (int)ActualHeight));
            _lastWidth = Math.Max(1, (int)ActualWidth);
            _lastHeight = Math.Max(1, (int)ActualHeight);
            CompositionTarget.Rendering += OnRendering;
            return new HandleRef(this, _childHandle);
        }
        catch
        {
            DestroyWindow(_childHandle);
            _childHandle = nint.Zero;
            throw;
        }
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        CompositionTarget.Rendering -= OnRendering;
        _presenter?.Dispose();
        _presenter = null;
        if (_childHandle != nint.Zero)
        {
            DestroyWindow(_childHandle);
            _childHandle = nint.Zero;
        }
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_presenter is null || _childHandle == nint.Zero)
            return;

        var width = Math.Max(1, (int)ActualWidth);
        var height = Math.Max(1, (int)ActualHeight);
        if (width != _lastWidth || height != _lastHeight)
        {
            _presenter.Resize(width, height);
            _lastWidth = width;
            _lastHeight = height;
        }
        _presenter.Render();
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateWindowEx(int exStyle, string className, string windowName,
        int style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint param);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint hwnd);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandle(string? moduleName);
}
