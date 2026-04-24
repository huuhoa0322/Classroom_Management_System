import { Component } from 'react';

export class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error, info) {
    console.error('Unhandled UI error', error, info);
  }

  handleReload = () => {
    window.location.reload();
  };

  render() {
    if (this.state.hasError) {
      return (
        <main className="min-h-screen bg-gray-50 px-6 py-12">
          <div className="mx-auto max-w-lg rounded-lg border border-red-200 bg-white p-6 shadow-sm">
            <h1 className="text-lg font-semibold text-gray-900">Không thể hiển thị màn hình</h1>
            <p className="mt-2 text-sm text-gray-600">
              Dữ liệu của bạn chưa bị thay đổi. Vui lòng tải lại trang để tiếp tục.
            </p>
            <button
              type="button"
              onClick={this.handleReload}
              className="mt-5 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
            >
              Tải lại trang
            </button>
          </div>
        </main>
      );
    }

    return this.props.children;
  }
}
