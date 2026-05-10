import { Component } from 'react';

export class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error, info) {
    console.error('Unhandled UI error', error, info);
  }

  handleReload = () => {
    window.location.reload();
  };

  handleRetry = () => {
    this.setState({ hasError: false, error: null });
  };

  /**
   * Kiểm tra lỗi có phải do mất kết nối mạng / server không phản hồi.
   */
  isNetworkError() {
    const { error } = this.state;
    if (!error) return false;
    const msg = error.message?.toLowerCase() || '';
    return (
      error.code === 'NETWORK_ERROR' ||
      error.code === 'TIMEOUT' ||
      error.code === 'ECONNABORTED' ||
      msg.includes('network') ||
      msg.includes('timeout') ||
      msg.includes('kết nối') ||
      msg.includes('failed to fetch')
    );
  }

  render() {
    if (this.state.hasError) {
      const isNetwork = this.isNetworkError();

      return (
        <main className="min-h-screen bg-gray-50 px-6 py-12">
          <div className={`mx-auto max-w-lg rounded-lg border p-6 shadow-sm ${
            isNetwork
              ? 'border-amber-200 bg-amber-50'
              : 'border-red-200 bg-white'
          }`}>
            {isNetwork ? (
              <>
                <div className="flex items-center gap-3 mb-3">
                  <span className="text-3xl">📡</span>
                  <h1 className="text-lg font-semibold text-gray-900">
                    Mất kết nối đến máy chủ
                  </h1>
                </div>
                <p className="text-sm text-gray-600">
                  Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng
                  của bạn hoặc thử lại sau.
                </p>
                <div className="mt-5 flex gap-3">
                  <button
                    type="button"
                    onClick={this.handleRetry}
                    className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white hover:bg-amber-700"
                  >
                    🔄 Thử lại
                  </button>
                  <button
                    type="button"
                    onClick={this.handleReload}
                    className="rounded-lg bg-gray-200 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-300"
                  >
                    Tải lại trang
                  </button>
                </div>
              </>
            ) : (
              <>
                <h1 className="text-lg font-semibold text-gray-900">
                  Không thể hiển thị màn hình
                </h1>
                <p className="mt-2 text-sm text-gray-600">
                  Dữ liệu của bạn chưa bị thay đổi. Vui lòng tải lại trang để tiếp tục.
                </p>
                <div className="mt-5 flex gap-3">
                  <button
                    type="button"
                    onClick={this.handleRetry}
                    className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
                  >
                    🔄 Thử lại
                  </button>
                  <button
                    type="button"
                    onClick={this.handleReload}
                    className="rounded-lg bg-gray-200 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-300"
                  >
                    Tải lại trang
                  </button>
                </div>
              </>
            )}
          </div>
        </main>
      );
    }

    return this.props.children;
  }
}

