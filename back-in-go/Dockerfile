FROM golang:1.27-alpine AS build
WORKDIR /app

COPY back-in-go/go.mod back-in-go/go.sum ./
RUN go mod download

COPY back-in-go/ ./

RUN go build -o server ./cmd/main.go

EXPOSE 8080

CMD ["./server"]
