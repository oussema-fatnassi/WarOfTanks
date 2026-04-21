package services

import (
	"errors"
	"time"

	"github.com/golang-jwt/jwt/v5"
)

// Claims is the payload embedded in access tokens.
type Claims struct {
	PlayerID string `json:"playerId"`
	Username string `json:"username"`
	jwt.RegisteredClaims
}

// RefreshClaims is the payload embedded in refresh tokens.
type RefreshClaims struct {
	PlayerID string `json:"playerId"`
	jwt.RegisteredClaims
}

// JWTService handles token generation and validation.
type JWTService struct {
	accessSecret  []byte
	refreshSecret []byte
}

// NewJWTService creates a JWTService from raw secret strings.
func NewJWTService(accessSecret, refreshSecret string) *JWTService {
	return &JWTService{
		accessSecret:  []byte(accessSecret),
		refreshSecret: []byte(refreshSecret),
	}
}

// GenerateAccessToken creates a signed JWT valid for 1 hour.
func (s *JWTService) GenerateAccessToken(playerID, username string) (string, error) {
	claims := Claims{
		PlayerID: playerID,
		Username: username,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(1 * time.Hour)),
			IssuedAt:  jwt.NewNumericDate(time.Now()),
		},
	}
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	return token.SignedString(s.accessSecret)
}

// GenerateRefreshToken creates a signed JWT valid for 7 days.
func (s *JWTService) GenerateRefreshToken(playerID string) (string, error) {
	claims := RefreshClaims{
		PlayerID: playerID,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(7 * 24 * time.Hour)),
			IssuedAt:  jwt.NewNumericDate(time.Now()),
		},
	}
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	return token.SignedString(s.refreshSecret)
}

// ValidateAccessToken parses and validates an access token, returning the claims.
func (s *JWTService) ValidateAccessToken(tokenStr string) (*Claims, error) {
	token, err := jwt.ParseWithClaims(tokenStr, &Claims{}, func(t *jwt.Token) (interface{}, error) {
		if _, ok := t.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, errors.New("unexpected signing method")
		}
		return s.accessSecret, nil
	})
	if err != nil {
		return nil, err
	}
	claims, ok := token.Claims.(*Claims)
	if !ok || !token.Valid {
		return nil, errors.New("invalid token")
	}
	return claims, nil
}

// ValidateRefreshToken parses and validates a refresh token, returning the claims.
func (s *JWTService) ValidateRefreshToken(tokenStr string) (*RefreshClaims, error) {
	token, err := jwt.ParseWithClaims(tokenStr, &RefreshClaims{}, func(t *jwt.Token) (interface{}, error) {
		if _, ok := t.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, errors.New("unexpected signing method")
		}
		return s.refreshSecret, nil
	})
	if err != nil {
		return nil, err
	}
	claims, ok := token.Claims.(*RefreshClaims)
	if !ok || !token.Valid {
		return nil, errors.New("invalid token")
	}
	return claims, nil
}
