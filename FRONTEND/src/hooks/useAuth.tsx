import { createContext, useContext, useState, type ReactNode } from "react";
import { setAccessToken as storeSetAccessToken } from '../auth/tokenStore'

interface AuthContextType {
    accessToken: string | null;
    setAccessToken: (token: string | null) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
    accessToken: null,
    setAccessToken: () => {},
    logout: () => {},
});

export const useAuth = () => {
    return useContext(AuthContext);
};

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const handleSetAccessToken = (token: string | null) => {
        storeSetAccessToken(token);   
        setAccessToken(token);
    };

    const logout = () => {
        handleSetAccessToken(null);
    };

    return (
        <AuthContext.Provider value={{ accessToken, setAccessToken: handleSetAccessToken, logout }}>
            {children}
        </AuthContext.Provider>
    );
}; 