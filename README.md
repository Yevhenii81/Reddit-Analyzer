Reddit Analyzer
Web API сервис для анализа постов из Reddit с поддержкой двух режимов парсинга:

Быстрый HTML-парсинг через HtmlAgilityPack (old.reddit.com)
Полноценный парсинг нового Reddit (www.reddit.com) через Playwright (headless-браузер)

Основные возможности

Два независимых эндпоинта: /api/reddit и /api/reddit/playwright
Фильтрация постов по ключевым словам в заголовке и в теле поста
Многопоточная обработка (Task.WhenAll)
Логирование в консоль и в файл logs/out.log (сохраняется на хосте)
Полностью Docker + Docker Compose

Инструкция по запуску

Убедитесь, что Docker Desktop запущен.
Склонируйте репозиторий:Bashgit clone <your-repo-url>
cd RedditAnalyzer
Запустите проект:Bashdocker-compose up --build

Сервис будет доступен по адресу:

http://localhost:5000

Swagger UI: http://localhost:5000/swagger

Примеры запросов

Быстрый режим (old.reddit.com)

httpPOST http://localhost:5000/api/reddit

Playwright режим (новый Reddit)

httpPOST http://localhost:5000/api/reddit/playwright

Тело запроса (для обоих эндпоинтов):

JSON{
  "items": [
    {
      "subreddit": "r/news",
      "keywords": ["war", "president"]
    },
    {
      "subreddit": "r/aww",
      "keywords": ["cat", "dog"]
    }
  ],
  "limit": 25
}
Пример ответа
JSON{
  "/r/news": [
    "UK weeks away from medicine shortages if Iran war continues",
    "French police thwart a suspected bombing in Paris"
  ],
  "/r/aww": [
    "My office has an open dog policy",
    "Random street cat cuddles are undefeated",
    "My cat thinks I am a pillow"
  ]
}
Описание использования

Сервис принимает список subreddit-ов и ключевых слов. Для каждого subreddit загружаются первые N постов, после чего они фильтруются по ключевым словам в заголовке и тексте поста. Запросы выполняются параллельно. Результат возвращается в виде словаря, где ключ — название subreddit, а значение — список подходящих заголовков.


Логи записываются в консоль и в папку logs/out.log (файл сохраняется вне контейнера благодаря Docker volume).

Теоретический вопрос


Какие проблемы могут возникнуть при получении данных через HTTP + парсинг HTML? Как бы вы их решали?

Основные проблемы:

Критичность структуры — любое обновление дизайна сайта ломает парсер.

Анти-бот защита — сайты блокируют headless-браузеры и автоматизированные запросы.

JavaScript-рендеринг — современные сайты (включая новый Reddit) загружают контент через JS. Обычный HTTP-запрос получает пустую страницу.

Низкая производительность — загрузка и парсинг большого HTML-документа занимает много времени.

Юридические ограничения — нарушение правил использования сайта.

Решения, реализованные в проекте:

Использование двух подходов: стабильный old.reddit.com + современный www.reddit.com через Playwright.

Мощное маскирование браузера в Playwright (User-Agent, viewport, init scripts, имитация поведения человека).

Параллельная обработка запросов через Task.WhenAll.

Возможность выбора между скоростью и актуальностью интерфейса.
