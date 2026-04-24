let _accessToken: string | null = null;

export const setAccessToken = (token: string | null) => {
    _accessToken = token;
};

export const getAccessToken = () => {
    return _accessToken;
};