<template lang="pug">
meta-list-card(
  title="High Scores"
  :item-list="highScores"
  :page-size="10"
  )
  template(#item-card="{ item: highScore, index }")
    MListItem(condensed)
      span(class="tw-flex-cols tw-flex")
        span(class="tw-w-8 tw-italic tw-text-neutral-400") {{ toOrdinalString(index + 1) }}
        span(v-if="highScore.name") {{ highScore.name }}
        span(
          v-else
          class="tw-italic tw-text-neutral-400"
          ) No Name

      template(#top-right) {{ highScore.score }}
</template>

<script lang="ts" setup>
import { computed } from 'vue'

import { getSinglePlayerSubscriptionOptions } from '@metaplay/core'
import { MetaListCard } from '@metaplay/meta-ui'
import { MCard, MListItem } from '@metaplay/meta-ui-next'
import { toOrdinalString } from '@metaplay/meta-utilities'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose high score data we want to show.
   */
  playerId: string
}>()

// Subscribe to the data we need to render this component.
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)

interface HighScoreData {
  name: string | null
  score: number
}

const highScores = computed((): HighScoreData[] => {
  return playerData.value?.model.playerData.highscores || []
})
</script>
