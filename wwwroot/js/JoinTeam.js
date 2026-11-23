async function getGameCookie(joinCode, joinRequest) {
  const response = await fetch(`/api/join/${joinCode}`, {
    method: "POST",
    credentials: "same-origin",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(joinRequest),
  });

  const resu = await response.json();

  return {
    succes: response.ok,
    error: resu.error,
  };
}
