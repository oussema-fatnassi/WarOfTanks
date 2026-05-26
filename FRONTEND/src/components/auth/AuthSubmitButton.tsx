import type { ButtonHTMLAttributes } from 'react'

interface AuthSubmitButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  isLoading: boolean
  label: string
  loadingLabel: string
}

const AuthSubmitButton = ({
  isLoading,
  label,
  loadingLabel,
  ...buttonProps
}: AuthSubmitButtonProps) => (
  <button
    className="flex min-h-[45px] w-full cursor-pointer items-center justify-between rounded bg-[#5ebc7b] px-4 text-sm font-bold text-[#0a0d10] transition hover:-translate-y-px hover:bg-[#71d391] disabled:cursor-not-allowed disabled:opacity-70 disabled:hover:translate-y-0 disabled:hover:bg-[#5ebc7b]"
    disabled={isLoading || buttonProps.disabled}
    type="submit"
    {...buttonProps}
  >
    <span>{isLoading ? loadingLabel : label}</span>
    <span aria-hidden="true">↵</span>
  </button>
)

export default AuthSubmitButton
