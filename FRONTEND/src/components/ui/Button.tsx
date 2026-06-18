import type { ButtonHTMLAttributes } from 'react'

type Variant = 'primary' | 'outline' | 'danger' | 'ghost'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
}

const base =
  'inline-flex items-center justify-center gap-2 rounded-card text-[13px] font-medium transition-colors disabled:pointer-events-none disabled:opacity-40'

const variants: Record<Variant, string> = {
  primary:
    'bg-win px-[18px] py-[11px] font-semibold tracking-[0.26px] text-on-accent hover:bg-win/90',
  outline:
    'border border-line px-4 py-[9px] text-fg hover:border-line-strong hover:bg-raised',
  danger:
    'border border-loss/60 px-4 py-[9px] text-loss hover:border-loss hover:bg-loss/10',
  ghost: 'px-3 py-2 text-muted hover:text-fg',
}

const Button = ({ variant = 'outline', className = '', ...props }: ButtonProps) => (
  <button className={`${base} ${variants[variant]} ${className}`} {...props} />
)

export default Button
