"use client"

import React from "react"
import { AlertTriangle } from "lucide-react"
import { Button } from "@/components/ui/button"

interface State {
  hasError: boolean
}

export class ErrorBoundary extends React.Component<React.PropsWithChildren, State> {
  state: State = { hasError: false }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-[320px] flex-col items-center justify-center gap-4 text-center">
          <AlertTriangle className="h-10 w-10 text-red-500" />
          <div>
            <h2 className="text-lg font-semibold">Bir şeyler ters gitti</h2>
            <p className="text-sm text-gray-500">Sayfayı yenileyip tekrar deneyin.</p>
          </div>
          <Button onClick={() => this.setState({ hasError: false })}>Tekrar Dene</Button>
        </div>
      )
    }

    return this.props.children
  }
}

