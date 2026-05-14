"use client"

import { Component, type ReactNode } from "react"
import { ErrorMessage } from "./ErrorMessage"

interface Props {
  children: ReactNode
}

interface State {
  hasError: boolean
}

export class ErrorBoundaryWrapper extends Component<Props, State> {
  state: State = { hasError: false }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  render() {
    if (this.state.hasError) return <ErrorMessage />
    return this.props.children
  }
}
