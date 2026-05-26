import type { InputHTMLAttributes } from 'react'

interface AuthTextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  hasError?: boolean
  label: string
  error?: string
}

const AuthTextField = ({
  hasError = false,
  label,
  error,
  className = '',
  ...inputProps
}: AuthTextFieldProps) => {
  const isInvalid = hasError || Boolean(error)
  const inputClassName = [
    'w-full rounded border bg-[#11161d] px-3.5 py-[13px] text-sm text-[#e7ecef] outline-none transition placeholder:text-[#6b7280] focus:border-[#5dcbd1] focus:shadow-[0_0_0_3px_rgba(93,203,209,0.16)]',
    isInvalid
      ? 'border-[#ee6951] shadow-[0_0_0_3px_rgba(238,105,81,0.18)]'
      : 'border-[#2a313b]',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <label className="flex flex-col gap-2">
      <span className="font-mono text-[10.5px] tracking-[1.05px] text-[#98a1ad]">
        {label}
      </span>
      <input className={inputClassName} {...inputProps} />
      {error && (
        <small className="text-xs leading-snug text-[#ee6951]">{error}</small>
      )}
    </label>
  )
}

export default AuthTextField
