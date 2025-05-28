import { Tag } from "./Tag"

export interface Photo {
    id: number
    url: string
    isMain: boolean
    createdAt: string
    username: string
    isApproved: boolean
    tags?: Tag[]
  }