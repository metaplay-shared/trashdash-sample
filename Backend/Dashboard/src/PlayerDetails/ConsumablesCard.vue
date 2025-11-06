<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
meta-list-card(
  title="Consumables"
  :item-list="allConsumables"
  )
  template(#item-card="{ item: consumable }")
    MListItem(condensed)
      template(#bottom-left)
        div(class="tw-flex tw-gap-x-1")
          img(
            :src="consumable.avatarUrl"
            class="tw-h-10 tw-w-10"
            )
          div
            div(class="") {{ consumable.displayName }}
            div(class="tw-italic tw-text-neutral-400") {{ consumable.description }}

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

// Subscribe to the data we need to render this component.
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)

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
</script>
