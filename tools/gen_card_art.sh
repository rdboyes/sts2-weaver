#!/usr/bin/env bash
# Usage: tools/gen_card_art.sh [repo dir] — writes placeholder card portraits for every Weaver card.
# A card's picture comes from tools/card_art/<id>.svg (1000x760, transparent background) when one exists.
set -euo pipefail
repo=${1:-$(dirname "$0")/..}
out=$repo/Weaver/images/card_portraits
font=/usr/share/fonts/noto/NotoSerif-Bold.ttf
tmp=$(mktemp -d)

# Woven thread overlay, shared by every card.
draw=()
for x in $(seq -760 50 1000); do
  draw+=(-stroke 'rgba(185,140,255,0.22)' -draw "line $x,0 $((x+760)),760")
  draw+=(-stroke 'rgba(185,140,255,0.12)' -draw "line $((x+760)),0 $x,760")
done
magick -size 1000x760 xc:none -strokewidth 6 "${draw[@]}" "$tmp/threads.png"

for f in "$repo"/WeaverCode/Cards/*.cs; do
  cls=$(basename "$f" .cs)
  type=$(grep -oP 'WeaverCard\([^,]+, CardType\.\K\w+' "$f" || true)
  [ -z "$type" ] && continue
  id=$(echo "$cls" | sed -E 's/([a-z0-9])([A-Z])/\1_\2/g' | tr '[:upper:]' '[:lower:]')
  title=$(grep -oP "\"WEAVER-${id^^}\.title\": \"\K[^\"]+" "$repo/Weaver/localization/eng/cards.json")
  case $type in
    Attack) top='#8c2433'; bot='#1e070d' ;;
    Skill)  top='#22587f'; bot='#081521' ;;
    Power)  top='#7a5a1c'; bot='#1f1406' ;;
  esac
  magick -size 1000x760 gradient:"$top-$bot" "$tmp/threads.png" -composite \
    \( -size 1000x760 -define gradient:extent=diagonal radial-gradient:'#ffffff-#3a3a3a' \) -compose multiply -composite "$tmp/bg.png"
  # Draw the card's picture from tools/card_art/<id>.svg, or fall back to its title.
  svg=$repo/tools/card_art/$id.svg
  if [ -f "$svg" ]; then
    rsvg-convert -w 1000 -h 760 "$svg" -o "$tmp/art.png"
    magick "$tmp/bg.png" "$tmp/art.png" -composite -strip "$out/big/$id.png"
  else
    magick "$tmp/bg.png" -font "$font" -pointsize 110 -gravity center \
      -fill 'rgba(0,0,0,0.6)' -annotate +6+6 "$title" \
      -fill '#f1e6ff' -stroke '#b98cff' -strokewidth 2 -annotate +0+0 "$title" \
      -strip "$out/big/$id.png"
  fi
  magick "$out/big/$id.png" -resize 250x190 -strip "$out/$id.png"
  echo "$type $id: $title"
done
rm -r "$tmp"
