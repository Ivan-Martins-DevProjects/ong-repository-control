package service

import (
	"errors"
	"reflect"
	"testing"
	"time"
	"uuid"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/repository"
)

type MockStockRepository struct {
	items      []domain.StockItem
	pagination *repository.Pagination
	listErr    error
	count      int64
	countErr   error
}

func (m *MockStockRepository) ListAllStockItems(
	pg *repository.Pagination,
) ([]domain.StockItem, *repository.Pagination, error) {
	if m.listErr != nil {
		return nil, nil, m.listErr
	}

	return m.items, m.pagination, nil
}

func (m *MockStockRepository) CountAllStockItems() (int64, error) {
	if m.countErr != nil {
		return 0, m.countErr
	}

	return m.count, nil
}

var responseItems = []domain.StockItem{
	{
		ID:          uuid.New(),
		Name:        "Arroz",
		Description: "Tipos variados de arroz 5Kg",
		Active:      true,
		CreatedAt:   time.Now(),
		UpdatedAt:   time.Now().Add(5 * time.Minute),
	},
	{
		ID:          uuid.New(),
		Name:        "Feijão",
		Description: "Tipos variados de feijão de 1Kg",
		Active:      true,
		CreatedAt:   time.Now(),
		UpdatedAt:   time.Now().Add(5 * time.Minute),
	},
}

func Test_stockService_ListAllStockItems(t *testing.T) {
	repositoryErr := errors.New("erro ao consultar estoque")

	tests := []struct {
		name     string
		page     int
		pageSize int

		repositoryItems      []domain.StockItem
		repositoryPagination *repository.Pagination
		repositoryCount      int64

		listErr  error
		countErr error

		want    *ListAllResponse
		wantErr bool
	}{
		{
			name:     "Listagem realizada com sucesso",
			page:     1,
			pageSize: 10,

			repositoryItems: responseItems,
			repositoryPagination: &repository.Pagination{
				Page:     1,
				PageSize: 10,
				Total:    2,
				Pages:    1,
				Offset:   0,
			},
			repositoryCount: 2,

			want: &ListAllResponse{
				Items: responseItems,
				Pages: &repository.Pagination{
					Page:     1,
					PageSize: 10,
					Total:    2,
					Pages:    1,
					Offset:   0,
				},
			},
		},
		{
			name:     "Listagem da segunda página realizada com sucesso",
			page:     2,
			pageSize: 10,

			repositoryItems: responseItems,
			repositoryPagination: &repository.Pagination{
				Page:     2,
				PageSize: 10,
				Total:    15,
				Pages:    2,
				Offset:   10,
			},
			repositoryCount: 15,

			want: &ListAllResponse{
				Items: responseItems,
				Pages: &repository.Pagination{
					Page:     2,
					PageSize: 10,
					Total:    15,
					Pages:    2,
					Offset:   10,
				},
			},
		},
		{
			name:     "Listagem da terceira página realizada com sucesso",
			page:     3,
			pageSize: 5,

			repositoryItems: responseItems,
			repositoryPagination: &repository.Pagination{
				Page:     3,
				PageSize: 5,
				Total:    12,
				Pages:    3,
				Offset:   10,
			},
			repositoryCount: 12,

			want: &ListAllResponse{
				Items: responseItems,
				Pages: &repository.Pagination{
					Page:     3,
					PageSize: 5,
					Total:    12,
					Pages:    3,
					Offset:   10,
				},
			},
		},
		{
			name:     "Lista vazia",
			page:     1,
			pageSize: 10,

			repositoryItems: []domain.StockItem{},
			repositoryPagination: &repository.Pagination{
				Page:     1,
				PageSize: 10,
				Total:    0,
				Pages:    0,
				Offset:   0,
			},
			repositoryCount: 0,

			want: &ListAllResponse{
				Items: []domain.StockItem{},
				Pages: &repository.Pagination{
					Page:     1,
					PageSize: 10,
					Total:    0,
					Pages:    0,
					Offset:   0,
				},
			},
		},
		{
			name:     "Erro ao listar itens do estoque",
			page:     1,
			pageSize: 10,

			listErr:  repositoryErr,
			countErr: nil,

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Erro ao contar itens do estoque",
			page:     1,
			pageSize: 10,

			listErr:  nil,
			countErr: repositoryErr,

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Página inválida",
			page:     0,
			pageSize: 10,

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Página negativa",
			page:     -1,
			pageSize: 10,

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Tamanho da página inválido",
			page:     1,
			pageSize: 0,

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Tamanho da página negativo",
			page:     1,
			pageSize: -10,

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Erro ao contar itens do estoque",
			page:     1,
			pageSize: 10,

			listErr:  nil,
			countErr: errors.New("erro ao contar itens"),

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Erro ao listar itens do estoque",
			page:     1,
			pageSize: 10,

			listErr:  errors.New("erro ao buscar itens"),
			countErr: nil,

			want:    nil,
			wantErr: true,
		},
		{
			name:     "Erro ao listar e contar itens",
			page:     1,
			pageSize: 10,

			listErr:  errors.New("erro ao buscar itens"),
			countErr: errors.New("erro ao contar itens"),

			want:    nil,
			wantErr: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			repo := &MockStockRepository{
				items:      tt.repositoryItems,
				pagination: tt.repositoryPagination,
				listErr:    tt.listErr,
				count:      tt.repositoryCount,
				countErr:   tt.countErr,
			}

			s := NewStockService(repo)

			got, err := s.ListAllStockItems(tt.page, tt.pageSize)

			if (err != nil) != tt.wantErr {
				t.Errorf(
					"ListAllStockItems() error = %v, wantErr %v",
					err,
					tt.wantErr,
				)
				return
			}

			if !reflect.DeepEqual(got, tt.want) {
				t.Errorf(
					"ListAllStockItems() = %v, want %v",
					got,
					tt.want,
				)
			}
		})
	}
}
