interface ErrorBannerProps {
  message: string
}

const ErrorBanner = ({ message }: ErrorBannerProps) => (
  <div className="mb-6 rounded-card border border-loss/40 bg-loss/10 px-4 py-3 text-sm text-loss">
    {message}
  </div>
)

export default ErrorBanner
