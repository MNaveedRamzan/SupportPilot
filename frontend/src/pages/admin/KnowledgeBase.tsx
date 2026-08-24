import { useEffect, useState, useCallback } from "react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/common/EmptyState";
import { ErrorState } from "@/components/common/ErrorState";
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
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const loadArticles = useCallback(() => {
    setIsLoading(true);
    setLoadError(null);
    fetchKnowledgeArticles()
      .then(setArticles)
      .catch(() => setLoadError("Failed to load knowledge base articles."))
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

  const handleAdd = async () => {
    if (!newText.trim()) return;

    setIsSaving(true);
    setActionError(null);
    try {
      await addKnowledgeArticle(newText.trim());
      setNewText("");
      loadArticles();
    } catch {
      setActionError("Failed to add article.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    setActionError(null);
    try {
      await deleteKnowledgeArticle(id);
      setArticles((prev) => prev.filter((a) => a.id !== id));
    } catch {
      setActionError("Failed to delete article.");
    }
  };

  if (loadError) {
    return <ErrorState message={loadError} onRetry={loadArticles} />;
  }

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

      {actionError && <p className="text-red-600 text-sm">{actionError}</p>}

      <div className="space-y-2">
        {isLoading ? (
          Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="flex items-start justify-between border rounded-md p-3">
              <Skeleton className="h-4 w-3/4" />
            </div>
          ))
        ) : articles.length === 0 ? (
          <EmptyState
            title="No articles yet"
            description="Add your first knowledge base article above."
          />
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