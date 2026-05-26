import { Link } from 'react-router-dom'

interface AuthSwitchLinkProps {
  label: string
  linkLabel: string
  to: string
}

const AuthSwitchLink = ({ label, linkLabel, to }: AuthSwitchLinkProps) => (
  <p className="mt-6 text-center text-sm text-[#98a1ad]">
    {label}{' '}
    <Link className="text-[#e7ecef] no-underline hover:text-[#5dcbd1]" to={to}>
      {linkLabel}
    </Link>
  </p>
)

export default AuthSwitchLink
