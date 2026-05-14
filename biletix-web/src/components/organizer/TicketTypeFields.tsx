"use client"

import type { Control, FieldErrors, UseFormRegister } from "react-hook-form"
import { useFieldArray } from "react-hook-form"
import { Plus, Trash2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import type { CreateEventFormData } from "@/lib/validations/event"

interface Props {
  control: Control<CreateEventFormData>
  register: UseFormRegister<CreateEventFormData>
  errors: FieldErrors<CreateEventFormData>
}

export function TicketTypeFields({ control, register, errors }: Props) {
  const { fields, append, remove } = useFieldArray({
    control,
    name: "ticketTypes",
  })

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <Label>Bilet Tipleri</Label>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => append({ name: "", price: 0, totalCapacity: 100 })}
          className="gap-1.5 border-red-200 text-red-500 hover:bg-red-50"
        >
          <Plus size={14} />
          Bilet Tipi Ekle
        </Button>
      </div>

      {fields.length === 0 && (
        <p className="rounded-lg border border-dashed py-4 text-center text-sm text-gray-400">
          En az bir bilet tipi ekleyin
        </p>
      )}

      {fields.map((field, index) => (
        <div key={field.id} className="space-y-3 rounded-xl border bg-gray-50 p-4">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-gray-700">Bilet Tipi {index + 1}</span>
            {fields.length > 1 && (
              <button
                type="button"
                onClick={() => remove(index)}
                className="text-red-400 transition-colors hover:text-red-600"
                aria-label="Bilet tipini sil"
              >
                <Trash2 size={15} />
              </button>
            )}
          </div>

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="space-y-1">
              <Label className="text-xs">Ad</Label>
              <Input placeholder="Standart, VIP..." {...register(`ticketTypes.${index}.name`)} />
              {errors.ticketTypes?.[index]?.name && (
                <p className="text-xs text-red-500">
                  {errors.ticketTypes[index]?.name?.message}
                </p>
              )}
            </div>

            <div className="space-y-1">
              <Label className="text-xs">Fiyat (₺)</Label>
              <Input
                type="number"
                placeholder="500"
                {...register(`ticketTypes.${index}.price`, {
                  valueAsNumber: true,
                })}
              />
              {errors.ticketTypes?.[index]?.price && (
                <p className="text-xs text-red-500">
                  {errors.ticketTypes[index]?.price?.message}
                </p>
              )}
            </div>

            <div className="space-y-1">
              <Label className="text-xs">Kapasite</Label>
              <Input
                type="number"
                placeholder="1000"
                {...register(`ticketTypes.${index}.totalCapacity`, {
                  valueAsNumber: true,
                })}
              />
              {errors.ticketTypes?.[index]?.totalCapacity && (
                <p className="text-xs text-red-500">
                  {errors.ticketTypes[index]?.totalCapacity?.message}
                </p>
              )}
            </div>
          </div>
        </div>
      ))}

      {errors.ticketTypes?.root && (
        <p className="text-xs text-red-500">{errors.ticketTypes.root.message}</p>
      )}
    </div>
  )
}
