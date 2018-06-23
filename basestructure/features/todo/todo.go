package todo

import (
	"net/http"

	"github.com/go-chi/chi"
	"github.com/go-chi/render"
)

type Todo struct {
	Slug  string `json:"slug"`
	Title string `json:"title"`
	Body  string `json:"body"`
}

func Routes() *chi.Mux {
	router := chi.NewRouter()
	router.Get("/{todoID}", GetATodo)
	router.Delete("/{todoID}", DeleteTodo)
	router.Post("/", CreateTodo)
	router.Get("/", GetAllTodos)
	return router
}

func GetATodo(w http.ResponseWriter, r *http.Request) {
	todoID := chi.URLParam(r, "todoID")
	todos := Todo{
		Slug:  todoID,
		Title: "Hello world",
		Body:  "Heloo world from planet earth",
	}
	render.JSON(w, r, todos) // A chi router helper for serializing and returning json
}

func DeleteTodo(w http.ResponseWriter, r *http.Request) {
	response := make(map[string]string)
	response["message"] = "Deleted TODO successfully"
	render.JSON(w, r, response) // Return some demo response
}

func CreateTodo(w http.ResponseWriter, r *http.Request) {
	response := make(map[string]string)
	response["message"] = "Created TODO successfully"
	render.JSON(w, r, response) // Return some demo response
}

func GetAllTodos(w http.ResponseWriter, r *http.Request) {
	todos := []Todo{
		{
			Slug:  "slug",
			Title: "Hello world",
			Body:  "Heloo world from planet earth",
		},
	}
	render.JSON(w, r, todos) // A chi router helper for serializing and returning json
}
