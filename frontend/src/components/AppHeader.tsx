import { Link } from "react-router-dom"
import { useAuth } from "../hooks/useAuth"
import { Button } from "@/components/ui/button"

export function AppHeader() {
  const { user, logout } = useAuth()

  if (!user) return null

  return (
    <div className="flex w-full max-w-2xl items-center justify-between px-2 text-sm text-muted-foreground">
      <span>
        {user.email} <span className="text-xs">({user.role})</span>
      </span>

      <div className="flex items-center gap-3">
        {user.role === "Admin" && (
          <Link to="/admin" className="hover:text-foreground">
            Admin Dashboard
          </Link>
        )}
        <Button variant="ghost" size="sm" onClick={logout}>
          Log out
        </Button>
      </div>
    </div>
  )
}