"use client"

import { Suspense } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import axios from "axios"
import Link from "next/link"
import { useSearchParams } from "next/navigation"
import { Ticket, Loader2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { useLogin } from "@/lib/hooks/useAuth"
import { loginSchema, type LoginFormData } from "@/lib/validations/auth"
import { useToast } from "@/hooks/use-toast"

function getLoginErrorMessage(error: unknown) {
  if (axios.isAxiosError<{ detail?: string }>(error)) {
    return error.response?.data?.detail || "Giriş başarısız"
  }

  return "Giriş başarısız"
}

function LoginForm() {
  const searchParams = useSearchParams()
  const registered = searchParams.get("registered")
  const { toast } = useToast()
  const login = useLogin()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  })

  const onSubmit = async (data: LoginFormData) => {
    login.mutate(data, {
      onError: (error) => {
        const message = getLoginErrorMessage(error)
        toast({ title: "Hata", description: message, variant: "destructive" })
      },
    })
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50 px-4">
      <div className="mb-8 flex items-center gap-2">
        <Ticket className="text-red-500" size={32} />
        <span className="text-2xl font-bold">Biletix</span>
      </div>

      {registered && (
        <div className="mb-4 w-full max-w-sm rounded-lg border border-green-200 bg-green-50 p-3 text-center text-sm text-green-700">
          Hesabınız oluşturuldu! Giriş yapabilirsiniz.
        </div>
      )}

      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle>Giriş Yap</CardTitle>
          <CardDescription>Hesabınızla devam edin.</CardDescription>
        </CardHeader>
        <CardContent>
          <form noValidate onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="email">E-posta</Label>
              <Input id="email" type="email" placeholder="ornek@email.com" {...register("email")} />
              {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
            </div>

            <div className="space-y-1">
              <Label htmlFor="password">Şifre</Label>
              <Input id="password" type="password" placeholder="••••••••" {...register("password")} />
              {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
            </div>

            <Button
              type="submit"
              className="w-full bg-red-500 hover:bg-red-600"
              disabled={login.isPending}
            >
              {login.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Giriş yapılıyor...
                </>
              ) : (
                "Giriş"
              )}
            </Button>
          </form>

          <p className="mt-4 text-center text-sm text-gray-500">
            Hesabınız yok mu?{" "}
            <Link href="/register" className="text-red-500 hover:underline">
              Kayıt olun
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  )
}
