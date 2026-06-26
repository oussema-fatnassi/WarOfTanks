package handlers

import "github.com/oussema-fatnassi/WarOfTanks/backend/models"

// ErrorResponse is the standard error payload returned by the API.
type ErrorResponse struct {
	Error string `json:"error" example:"invalid request"`
}

// MessageResponse is a generic success message payload.
type MessageResponse struct {
	Message string `json:"message" example:"logged out"`
}

// HealthResponse is returned by health check endpoints.
type HealthResponse struct {
	Status string `json:"status" example:"ok"`
}

// RegisterResponse is returned after successful player registration.
type RegisterResponse struct {
	ID       string `json:"id" example:"665f1a9fbad63deb45c6e001"`
	Username string `json:"username" example:"tank_commander"`
	Email    string `json:"email" example:"tank@example.com"`
}

// LoginResponse contains the access token and safe player profile.
type LoginResponse struct {
	AccessToken string        `json:"accessToken" example:"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."`
	Player      models.Player `json:"player"`
}

// RefreshResponse contains a new short-lived access token.
type RefreshResponse struct {
	AccessToken string `json:"accessToken" example:"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."`
}
