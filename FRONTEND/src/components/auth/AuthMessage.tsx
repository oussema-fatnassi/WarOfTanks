type AuthMessageVariant = 'error' | 'success'

interface AuthMessageProps {
  children: string
  className?: string
  variant: AuthMessageVariant
}

const variantClasses: Record<AuthMessageVariant, string> = {
  error:
    "relative pl-4 text-xs leading-snug text-[#ee6951] before:absolute before:top-[0.65em] before:left-0 before:size-1.5 before:-translate-y-1/2 before:rounded-full before:bg-[#ee6951] before:content-['']",
  success:
    'rounded border border-[rgba(94,188,123,0.45)] bg-[rgba(94,188,123,0.10)] px-3.5 py-3 text-[13px] text-[#8fe0a9]',
}

const AuthMessage = ({
  children,
  className = '',
  variant,
}: AuthMessageProps) => (
  <p className={`${variantClasses[variant]} ${className}`}>{children}</p>
)

export default AuthMessage
