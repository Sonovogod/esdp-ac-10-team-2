export default [
  {
    text: "Home",
    path: "/home",
    icon: "home"
  },
  {
    text: "Администрирование",
    icon: "preferences",
    items: [
      {
        text: "Profile",
        path: "/profile"
      },
      {
        text: "Users",
        path: "/users"
      },
      {
        text: "Users Table",
        path: "/users_table"
      },
      {
        text: "Журнал контрагентов",
        path: "/ContrAgentsJournal"
      }
    ]
  },
  {
    text: "Работа с проектами",
    icon: "folder",
    items: [
      {
        text: "Проекты",
        path: "/projects"
      },
      {
        text: "Новый проект",
        path: "/create-projects"
      }
    ]
  },
  {
    text: "Антенны",
    icon: "folder",
    items: [
      {
        text: "Список антенн",
        path: "/antennae_table"
      },
    ]
  }
  ];
