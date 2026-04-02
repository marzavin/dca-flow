FROM node:20-alpine AS builder

WORKDIR /app
RUN npm install -g pnpm

COPY . .

WORKDIR /app/frontend
RUN pnpm install && pnpm build

WORKDIR /app/backend
RUN pnpm install && pnpm build

RUN npx prisma generate

FROM node:20-alpine

WORKDIR /app

COPY --from=builder /app/backend/dist ./dist
COPY --from=builder /app/backend/node_modules ./node_modules
COPY --from=builder /app/frontend/dist ./public
COPY --from=builder /app/prisma ./prisma

EXPOSE 3000

CMD ["node", "dist/index.js"]
