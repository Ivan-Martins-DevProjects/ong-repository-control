package service_test

import (
	"strings"
	"testing"
	"uuid"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
	dto "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/DTO"
	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/repository"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/service"
)

type MockUserRepository struct {
	users []domain.User
}

func (m *MockUserRepository) FindByEmail(email string) (*domain.User, error) {
	return nil, nil
}

func (m *MockUserRepository) CreateUser(user domain.User) (string, error) {
	for _, value := range m.users {
		if user.Email == value.Email {
			return "", apperror.BadRequest("Email já está cadastrado", nil)
		}

		if strings.TrimSpace(user.Password) == "" {
			return "", apperror.BadRequest("Senha não informada, erro ao criar usuário", nil)
		}
	}

	m.users = append(m.users, domain.User{
		ID:       uuid.New(),
		Name:     user.Name,
		Email:    user.Email,
		Password: user.Password,
	})

	return m.users[len(m.users)-1].ID.String(), nil
}

func TestAuthService_CreateUser(t *testing.T) {
	users := []domain.User{
		{
			ID:       uuid.New(),
			Name:     "Ivan Martins",
			Email:    "ivan@email.com",
			Password: "teste",
		},
	}

	userRepo := &MockUserRepository{
		users: users,
	}
	secret := "123456"

	userTestDefault := &dto.CreateUserRequest{
		Name:     "Ivan Martins Teste",
		Email:    "ivan.teste@email.com",
		Password: "testandoservice",
	}

	tests := []struct {
		name            string
		userRepo        repository.UserRepository
		secret          string
		user            dto.CreateUserRequest
		wantErr         bool
		expectedMessage string
	}{
		{
			name:     "Usuário criado com sucesso.",
			userRepo: userRepo,
			secret:   secret,
			user:     *userTestDefault,
			wantErr:  false,
		},
		{
			name:            "Usuário duplicado",
			userRepo:        userRepo,
			secret:          secret,
			user:            *userTestDefault,
			wantErr:         true,
			expectedMessage: "Email já está cadastrado",
		},
		{
			name:     "Nome com vários espaços",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     "    ",
				Email:    userTestDefault.Email,
				Password: userTestDefault.Password,
			},
			wantErr:         true,
			expectedMessage: "Não permitido o cadastro sem um nome válida",
		},
		{
			name:     "Senha com vários espaços",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     userTestDefault.Name,
				Email:    userTestDefault.Email,
				Password: "          ",
			},
			wantErr:         true,
			expectedMessage: "Não permitido o cadastro sem uma senha válida",
		},
		{
			name:     "Usuário com email vazio",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     userTestDefault.Name,
				Email:    "    ",
				Password: userTestDefault.Password,
			},
			wantErr:         true,
			expectedMessage: "O campo 'Email' é inválido",
		},
		{
			name:     "Nome muito curto (menos de 3 caracteres)",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     "Ab",
				Email:    "novo_curto@email.com",
				Password: userTestDefault.Password,
			},
			wantErr:         true,
			expectedMessage: "O campo 'Name' é inválido",
		},
		{
			name:     "Nome muito longo (mais de 50 caracteres)",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     "NomeExtremamenteLongoParaTestarALimitacaoDeCaracteresPermitidaPelaRegraMaxDaStructDoDTOComCertezaPassaDeCinquenta",
				Email:    "novo_longo@email.com",
				Password: userTestDefault.Password,
			},
			wantErr:         true,
			expectedMessage: "O campo 'Name' é inválido",
		},
		{
			name:     "Email com formato inválido",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     userTestDefault.Name,
				Email:    "email-invalido-sem-arroba",
				Password: userTestDefault.Password,
			},
			wantErr:         true,
			expectedMessage: "O campo 'Email' é inválido",
		},
		{
			name:     "Senha muito curta (menos de 8 caracteres)",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     userTestDefault.Name,
				Email:    "senha_curta@email.com",
				Password: "123",
			},
			wantErr:         true,
			expectedMessage: "O campo 'Password' é inválido",
		},
		{
			name:     "Nome totalmente vazio",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     "",
				Email:    "nome_vazio@email.com",
				Password: userTestDefault.Password,
			},
			wantErr:         true,
			expectedMessage: "O campo 'Name' é inválido",
		},
		{
			name:     "Todos os campos obrigatórios vazios",
			userRepo: userRepo,
			secret:   secret,
			user: dto.CreateUserRequest{
				Name:     "",
				Email:    "",
				Password: "",
			},
			wantErr:         true,
			expectedMessage: "O campo 'Name' é inválido",
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			s := service.NewAuthService(tt.userRepo, tt.secret)
			id, gotErr := s.CreateUser(tt.user)
			if gotErr != nil {
				if !tt.wantErr {
					t.Errorf("Erro ao criar usuário: %v", gotErr)
					return
				}

				if tt.expectedMessage != "" && !strings.Contains(gotErr.Error(), tt.expectedMessage) {
					t.Errorf("Esperava a mensagem contendo: %q\n e obteve %q", tt.expectedMessage, gotErr.Error())
				}
				return
			}

			if tt.wantErr {
				t.Error("CreateUser obteve sucesso inesperadamente")
				return
			}

			_, err := uuid.Parse(id)
			if err != nil {
				t.Error("Erro ao realizar o parse do UUID retornado pelo banco de dados")
			}
		})
	}
}
