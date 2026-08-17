import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import {
  fetchKnowledgeArticles,
  addKnowledgeArticle,
  deleteKnowledgeArticle,
  type KnowledgeArticle as KnowledgeArticleType,
} from "@/api/dashboard";

/**
 * Knowledge base management — list, add, and delete articles directly in
 * the vector store (Day 3 decision: no separate Postgres mirror table).
 */
export function KnowledgeBase() {
  const [articles, setArticles] = useState<KnowledgeArticleType[]>([]);
  const [newText, setNewText] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadArticles = () => {
    fetchKnowledgeArticles()
      .then(setArticles)
      .catch(() => setError("Failed to load knowledge base articles."));
  };

  useEffect(() => {
    loadArticles();
  }, []);

  const handleAdd = async () => {
    if (!newText.trim()) return;

    setIsSaving(true);
    try {
      await addKnowledgeArticle(newText.trim());
      setNewText("");
      loadArticles();
    } catch {
      setError("Failed to add article.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteKnowledgeArticle(id);
      setArticles((prev) => prev.filter((a) => a.id !== id));
    } catch {
      setError("Failed to delete article.");
    }
  };

  return (
    <div className="mt-6 space-y-6">
      <div className="flex gap-2">
        <textarea
          className="flex-1 border rounded-md p-2 text-sm resize-none min-h-[80px]"
          placeholder="New knowledge base article text..."
          value={newText}
          onChange={(e) => setNewText(e.target.value)}
        />
        <Button onClick={handleAdd} disabled={isSaving || !newText.trim()}>
          {isSaving ? "Adding..." : "Add"}
        </Button>
      </div>

      {error && <p className="text-red-600 text-sm">{error}</p>}

      <div className="space-y-2">
        {articles.length === 0 ? (
          <p className="text-gray-500 text-sm">No articles yet.</p>
        ) : (
          articles.map((article) => (
            <div
              key={article.id}
              className="flex items-start justify-between border rounded-md p-3"
            >
              <p className="text-sm text-gray-700 flex-1">{article.text}</p>
              <Button
                variant="ghost"
                size="sm"
                className="text-red-600 hover:text-red-700"
                onClick={() => handleDelete(article.id)}
              >
                Delete
              </Button>
            </div>
          ))
        )}
      </div>
    </div>
  );
}