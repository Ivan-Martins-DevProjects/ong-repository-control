package token

import "github.com/golang-jwt/jwt/v5"

type Token interface {
	generateClaims() jwt.MapClaims
	GenerateToken(secret []byte) error
	GetValue() string
}
