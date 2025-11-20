<template lang="pug">
// Create a new card with a list of items
meta-list-card(
  title="Consumables"
  :item-list="allConsumables"
  )
  // For each item...
  template(#item-card="{ item: consumable }")
    MListItem(condensed)
      // Render an avatar, name, and description in the bottom left section
      template(#bottom-left)
        div(class="tw-flex tw-gap-x-1")
          img(
            :src="consumable.avatarUrl"
            class="tw-h-10 tw-w-10"
            )
          div
            div(class="") {{ consumable.displayName }}
            div(class="tw-italic tw-text-neutral-400") {{ consumable.description }}

      // Render the amount of consumables in the bottom right section
      template(#bottom-right)
        div(v-if="consumable.amount") {{ consumable.amount }}
        div(
          v-else
          class="tw-italic tw-text-neutral-400"
          ) None
</template>

<script lang="ts" setup>
import { computed } from 'vue'

import { getSinglePlayerSubscriptionOptions, isEpochTime } from '@metaplay/core'
import { MetaListCard } from '@metaplay/meta-ui'
import { MCard, MListItem } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose consumable data we want to show.
   */
  playerId: string
}>()

// Use Metaplay's built in data fetching utility to fetch the player's data.
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)

// Use Vue's Computed to automatically trigger an update when the player's data changes
const allConsumables = computed((): ConsumableDisplayData[] => {
  return allConsumableDetails.map((consumableDetails) => {
    return {
      displayName: consumableDetails.displayName,
      description: consumableDetails.description,
      avatarUrl: consumableDetails.avatarUrl,
      amount: playerData.value?.model.playerData.consumables[consumableDetails.id] || 0,
    }
  })
})

interface ConsumableDetails {
  id: string
  displayName: string
  description: string
  avatarUrl: string
}

const allConsumableDetails: ConsumableDetails[] = [
  {
    id: 'COIN_MAG',
    displayName: 'Coin Magnet',
    description: 'Attracts coins towards you for a limited time',
    avatarUrl: '/Consumables/COIN_MAG.png',
  },
  {
    id: 'SCORE_MULTIPLAYER',
    displayName: 'Score Multiplier',
    description: 'Doubles your score for a limited time',
    avatarUrl: '/Consumables/SCORE_MULTIPLAYER.png',
  },
  {
    id: 'INVINCIBILITY',
    displayName: 'Invincibility',
    description: 'Makes you invincible for a limited time',
    avatarUrl: '/Consumables/INVINCIBILITY.png',
  },
  {
    id: 'EXTRALIFE',
    displayName: 'Extra Life',
    description: 'Grants an extra life',
    avatarUrl: '/Consumables/EXTRALIFE.png',
  },
]

interface ConsumableDisplayData {
  displayName: string
  description: string
  avatarUrl: string
  amount: number
}

</script>
