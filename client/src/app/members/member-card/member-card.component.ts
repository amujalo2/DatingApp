import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../_models/member';
import { RouterLink } from '@angular/router';
import { LikesService } from '../../_services/likes.service';
import { NgIf } from '@angular/common';
import { PresenceService } from '../../_services/presence.service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, NgIf],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css',
})
export class MemberCardComponent {
  member = input.required<Member>();
  private presenceService = inject(PresenceService);
  private likesService = inject(LikesService);
  hasLiked = computed(() => this.likesService.likeIds().includes(this.member().id));
  isOnline = computed(() => this.presenceService.onlineUsers().includes(this.member().username));
  toggleLike() {
    this.likesService.toggleLike(this.member().id).subscribe({
      next: () => {
        if(this.hasLiked()) {
          this.likesService.likeIds.update(ids => ids.filter(x => x != this.member().id))
        } else {
          this.likesService.likeIds.update(ids => [...ids, this.member().id])
        }
      }
    });
  }
}
