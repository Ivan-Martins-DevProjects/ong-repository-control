package service

import (
	"fmt"
	"strings"
	"uuid"

	"github.com/go-playground/validator/v10"
	"golang.org/x/crypto/bcrypt"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
	dto "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/DTO"
	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/token"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/repository"
)

type AuthService struct {
	userRepo  repository.UserRepository
	jwtSecret []byte
}

func NewAuthService(userRepo repository.UserRepository, secret string) *AuthService {
	return &AuthService{
		userRepo:  userRepo,
		jwtSecret: []byte(secret),
	}
}

func hashPassword(password string) (string, error) {
	bytes, err := bcrypt.GenerateFromPassword([]byte(password), 10)
	return string(bytes), err
}

func (s *AuthService) CreateUser(user dto.CreateUserRequest) (token.Token, error) {
	validate := validator.New(validator.WithRequiredStructEnabled())
	err := validate.Struct(user)
	if err != nil {
		if validationErrs, ok := err.(validator.ValidationErrors); ok {
			var fields []string
			for _, e := range validationErrs {
				fields = append(fields, e.Field())
			}

			fullMessage := fmt.Sprintf("Os campos: %v estão inválidos", strings.Join(fields, " e "))
			return nil, apperror.BadRequest(fullMessage, err)
		}
		return nil, apperror.BadRequest("Erro de validação desconhecido", err)
	}

	if strings.TrimSpace(user.Name) == "" {
		return nil, apperror.BadRequest("Não permitido o cadastro sem um nome válida", nil)
	}

	if strings.TrimSpace(user.Password) == "" {
		return nil, apperror.BadRequest("Não permitido o cadastro sem uma senha válida", nil)
	}

	passwordHash, err := hashPassword(user.Password)
	if err != nil {
		return nil, apperror.InternalServerError("Erro ao gerar hash de senha do usuário", err)
	}

	data := domain.User{
		ID:       uuid.NewV4(),
		Name:     user.Name,
		Email:    user.Email,
		Password: passwordHash,
	}

	err = validate.Struct(data)
	if err != nil {
		for _, err := range err.(validator.ValidationErrors) {
			message := fmt.Sprintf("O campo '%s' é inválido", err.Field())
			return nil, apperror.BadRequest(message, err)
		}
	}

	id, err := s.userRepo.CreateUser(data)
	if err != nil {
		return nil, err
	}

	token := token.NewJwtToken(
		id,
		token.HS256,
		token.ExpireIn8,
	)

	err = token.GenerateToken(s.jwtSecret)
	if err != nil {
		return nil, err
	}

	return token, nil
}

func (s *AuthService) Login(email, password string) (token.Token, error) {
	user, err := s.userRepo.FindByEmail(email)
	if err != nil {
		return nil, apperror.Unauthorized("Credenciais inválidas", nil)
	}

	err = bcrypt.CompareHashAndPassword([]byte(user.Password), []byte(password))
	if err != nil {
		return nil, apperror.Unauthorized("Credenciais inválidas", nil)
	}

	token := token.NewJwtToken(
		user.ID.String(),
		token.HS256,
		token.ExpireIn8,
	)

	err = token.GenerateToken(s.jwtSecret)
	if err != nil {
		return nil, err
	}

	return token, nil
}
