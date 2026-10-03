package domain

import (
	"time"
	"uuid"

	"gorm.io/gorm"
)

type User struct {
	ID        uuid.UUID      `json:"id" gorm:"type:uuid;primaryKey"`
	Name      string         `json:"name" binding:"required" gorm:"not null"`
	Email     string         `json:"email" binding:"required" gorm:"not null;unique"`
	Password  string         `json:"-" binding:"required" gorm:"not null"`
	Active    bool           `json:"active" binding:"required" gorm:"default:true;not null" `
	CreatedAt time.Time      `json:"created_at"`
	UpdatedAt time.Time      `json:"updated_at"`
	DeletedAt gorm.DeletedAt `json:"-" gorm:"index"`
}

func (u *User) BeforeCreate(tx *gorm.DB) (err error) {
	if u.ID == uuid.New() {
		u.ID = uuid.New()
	}

	return
}
