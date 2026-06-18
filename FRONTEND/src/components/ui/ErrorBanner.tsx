interface ErrorBannerProps {
  message: string
}

const ErrorBanner = ({ message }: ErrorBannerProps) => (
  <div className="mb-6 border border-[#ee6951]/40 bg-[#ee6951]/10 px-4 py-3 text-sm text-[#ee6951]">
    {message}
  </div>
)

export default ErrorBanner
