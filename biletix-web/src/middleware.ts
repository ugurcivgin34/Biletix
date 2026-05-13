import { NextResponse } from "next/server"
import type { NextRequest } from "next/server"

const protectedRoutes = ["/my-tickets", "/profile", "/checkout", "/organizer", "/admin"]
const authRoutes = ["/login", "/register"]

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl
  const token = request.cookies.get("accessToken")?.value

  const isProtected = protectedRoutes.some((route) => pathname.startsWith(route))
  if (isProtected && !token) {
    return NextResponse.redirect(new URL(`/login?from=${pathname}`, request.url))
  }

  const isAuthRoute = authRoutes.some((route) => pathname.startsWith(route))
  if (isAuthRoute && token) {
    return NextResponse.redirect(new URL("/", request.url))
  }

  return NextResponse.next()
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
}
