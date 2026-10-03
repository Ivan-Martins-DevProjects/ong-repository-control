package token

import (
	"fmt"

	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
	"github.com/golang-jwt/jwt/v5"
)

type JwtToken struct {
	UserID string
	Value  string
	Exp    int64
	Method string
}

func NewJwtToken(userID, method string, exp int64) *JwtToken {
	return &JwtToken{
		UserID: userID,
		Exp:    exp,
		Method: method,
	}
}

func (j *JwtToken) GetUserID() string {
	return j.UserID
}

func (j *JwtToken) GetValue() string {
	return j.Value
}

func (j *JwtToken) GenerateToken(secret []byte) error {
	methods := map[string]jwt.SigningMethod{
		"HS256": jwt.SigningMethodHS256,
	}

	actualMethod, exists := methods[j.Method]

	if !exists {
		message := fmt.Sprintf("Método não disponível para utilização: %s", j.Method)
		return apperror.InternalServerError(message, nil)

	}

	token := jwt.NewWithClaims(actualMethod, j.generateClaims())
	tokenString, err := token.SignedString(secret)
	if err != nil {
		return apperror.InternalServerError("Erro ao gerar token de autenticação", err)
	}

	j.Value = tokenString

	return nil
}

func (j *JwtToken) generateClaims() jwt.MapClaims {
	return jwt.MapClaims{
		"user_id": j.UserID,
		"exp":     j.Exp,
	}
}
