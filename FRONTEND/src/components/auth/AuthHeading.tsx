interface AuthHeadingProps {
  title: string
  subtitle: string
}

const AuthHeading = ({ title, subtitle }: AuthHeadingProps) => (
  <div className="mb-8">
    <h1 className="m-0 text-[28px] leading-tight font-semibold tracking-normal text-[#e7ecef]">
      {title}
    </h1>
    <p className="mt-2.5 text-sm text-[#98a1ad]">{subtitle}</p>
  </div>
)

export default AuthHeading
