const TOKEN_KEY = 'wot_access_token'

let _accessToken: string | null = localStorage.getItem(TOKEN_KEY)

export const setAccessToken = (token: string | null) => {
  _accessToken = token
  if (token) localStorage.setItem(TOKEN_KEY, token)
  else localStorage.removeItem(TOKEN_KEY)
}

export const getAccessToken = () => {
  return _accessToken
}
