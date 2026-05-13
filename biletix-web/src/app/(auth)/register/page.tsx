"use client"

import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import axios from "axios"
import Link from "next/link"
import { Ticket, Loader2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { useRegister } from "@/lib/hooks/useAuth"
import { registerSchema, type RegisterFormData } from "@/lib/validations/auth"
import { useToast } from "@/hooks/use-toast"

interface RegisterErrorResponse {
  detail?: string
  errors?: {
    Email?: string[]
  }
}

function getRegisterErrorMessage(error: unknown) {
  if (axios.isAxiosError<RegisterErrorResponse>(error)) {
    return (
      error.response?.data?.detail ||
      error.response?.data?.errors?.Email?.[0] ||
      "Kayıt başarısız"
    )
  }

  return "Kayıt başarısız"
}

export default function RegisterPage() {
  const { toast } = useToast()
  const registerMutation = useRegister()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  })

  const onSubmit = (data: RegisterFormData) => {
    registerMutation.mutate(data, {
      onError: (error) => {
        const message = getRegisterErrorMessage(error)
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

      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle>Kayıt Ol</CardTitle>
          <CardDescription>Yeni Biletix hesabınızı oluşturun.</CardDescription>
        </CardHeader>
        <CardContent>
          <form noValidate onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label htmlFor="firstName">Ad</Label>
                <Input id="firstName" placeholder="Ahmet" {...register("firstName")} />
                {errors.firstName && <p className="text-xs text-red-500">{errors.firstName.message}</p>}
              </div>
              <div className="space-y-1">
                <Label htmlFor="lastName">Soyad</Label>
                <Input id="lastName" placeholder="Yılmaz" {...register("lastName")} />
                {errors.lastName && <p className="text-xs text-red-500">{errors.lastName.message}</p>}
              </div>
            </div>

            <div className="space-y-1">
              <Label htmlFor="email">E-posta</Label>
              <Input id="email" type="email" placeholder="ornek@email.com" {...register("email")} />
              {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
            </div>

            <div className="space-y-1">
              <Label htmlFor="password">Şifre</Label>
              <Input id="password" type="password" placeholder="••••••••" {...register("password")} />
              {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
              <p className="text-xs text-gray-400">En az 8 karakter, büyük/küçük harf ve rakam</p>
            </div>

            <Button
              type="submit"
              className="w-full bg-red-500 hover:bg-red-600"
              disabled={registerMutation.isPending}
            >
              {registerMutation.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Kayıt yapılıyor...
                </>
              ) : (
                "Kayıt Ol"
              )}
            </Button>
          </form>

          <p className="mt-4 text-center text-sm text-gray-500">
            Zaten hesabınız var mı?{" "}
            <Link href="/login" className="text-red-500 hover:underline">
              Giriş yapın
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
